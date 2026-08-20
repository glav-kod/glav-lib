#!/usr/bin/env node
// Claude Code Stop hook: собирает решение, когда агент остановился после правок C#-кода,
// и, если сборка упала, блокирует остановку и отдаёт агенту вывод компилятора, чтобы он
// починил код и продолжил работу.
//
// Проверка ровно та же, что требует `.claude/rules/agent-verification.md`, — хук нужен затем,
// чтобы её нельзя было забыть. В `Directory.Build.props` включён `TreatWarningsAsErrors`,
// поэтому сборка ловит и предупреждения.

import { execSync, spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { homedir } from "node:os";

const BUILD_FILE_RE = /\.(cs|csproj|props|targets|sln|editorconfig)$/;
const SOLUTION = "GlavLib.sln";
const GENERATORS_PREFIX = "GlavLib.SourceGenerators/";
const GENERATORS_TESTS = "GlavLib.SourceGenerators.Tests/GlavLib.SourceGenerators.Tests.csproj";
const MAX_LEN = 12000;

function readHookInput() {
  try {
    return JSON.parse(readFileSync(0, "utf8"));
  } catch {
    return {};
  }
}

function getRepoRoot() {
  const cwd = process.env.CLAUDE_PROJECT_DIR || ".";
  try {
    return execSync("git rev-parse --show-toplevel", {
      cwd,
      encoding: "utf8",
      stdio: ["ignore", "pipe", "ignore"],
    }).trim();
  } catch {
    return cwd;
  }
}

function collectChangedFiles(repoRoot) {
  const files = new Set();
  const commands = [
    "git diff --name-only HEAD",
    "git diff --cached --name-only",
    "git ls-files --others --exclude-standard",
  ];

  for (const command of commands) {
    try {
      const output = execSync(command, {
        cwd: repoRoot,
        encoding: "utf8",
        stdio: ["ignore", "pipe", "ignore"],
      });
      for (const line of output.split(/\r?\n/)) {
        const file = line.trim();
        if (file && BUILD_FILE_RE.test(file)) {
          files.add(file);
        }
      }
    } catch {
      // ошибки git игнорируем: без списка файлов хук просто не срабатывает
    }
  }

  return [...files].sort();
}

// SDK в облачном контейнере ставит SessionStart-хук, и в PATH самого Stop-хука его может
// не быть. Ищем в тех же местах, куда он ставится; не нашли — молча выходим: хук существует
// ради проверки, а не ради того, чтобы ругаться на отсутствие окружения.
function resolveDotnet() {
  const candidates = [
    process.env.DOTNET_ROOT ? join(process.env.DOTNET_ROOT, "dotnet") : null,
    "/root/.dotnet/dotnet",
    join(homedir(), ".dotnet", "dotnet"),
    "/usr/share/dotnet/dotnet",
    "/usr/local/share/dotnet/dotnet",
  ].filter(Boolean);

  for (const candidate of candidates) {
    if (existsSync(candidate)) {
      return candidate;
    }
  }

  const probe = spawnSync("dotnet", ["--version"], { stdio: "ignore", shell: true });
  return probe.status === 0 ? "dotnet" : null;
}

function run(dotnet, args, cwd) {
  const result = spawnSync(dotnet, args, {
    cwd,
    encoding: "utf8",
    stdio: ["ignore", "pipe", "pipe"],
    env: { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: "1", DOTNET_NOLOGO: "1" },
  });

  if (result.status === 0) {
    return null;
  }

  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`.trimEnd();
  return `dotnet ${args.join(" ")} завершилась с кодом ${result.status ?? "unknown"}\n\n${output}`;
}

const hookInput = readHookInput();

// Защита от зацикливания: если мы уже перезапущены предыдущим Stop-хуком, второй раз
// не блокируем.
if (hookInput.stop_hook_active) {
  process.exit(0);
}

const repoRoot = getRepoRoot();
const changedFiles = collectChangedFiles(repoRoot);

if (changedFiles.length === 0) {
  process.exit(0);
}

const dotnet = resolveDotnet();
if (dotnet === null) {
  process.exit(0);
}

const failures = [];

const buildFailure = run(dotnet, ["build", SOLUTION, "-c", "Debug"], repoRoot);
if (buildFailure !== null) {
  failures.push(buildFailure);
}

// Тесты генераторов сравнивают выпущенный код с эталонной строкой посимвольно, поэтому
// ломаются от любой правки шаблона. Прогоняем их только когда генераторы менялись и только
// после успешной сборки: собирать решение второй раз незачем.
if (buildFailure === null && changedFiles.some((file) => file.startsWith(GENERATORS_PREFIX))) {
  const testsFailure = run(dotnet, ["test", GENERATORS_TESTS, "--no-build"], repoRoot);
  if (testsFailure !== null) {
    failures.push(testsFailure);
  }
}

if (failures.length === 0) {
  process.exit(0);
}

let combined = failures.join("\n\n---\n\n");
if (combined.length > MAX_LEN) {
  combined = `${combined.slice(0, MAX_LEN)}\n\n... (вывод обрезан)`;
}

const reason = `Проверка после последнего хода агента не прошла. Почини перечисленное ниже и прогони проверку заново.

${combined}`;

console.log(JSON.stringify({ decision: "block", reason }));
process.exit(0);
