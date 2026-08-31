import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildSteps.script
import jetbrains.buildServer.configs.kotlin.triggers.finishBuildTrigger
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
 * Пайплайн выпуска пакетов GlavLib.
 *
 * Состоит из двух конфигураций: `Pack` собирает решение в контейнере и упаковывает пакеты,
 * `Publish` публикует то, что собрала `Pack`. Вся работа делается скриптами из каталога `ci`,
 * поэтому шаг TeamCity сводится к одной команде, а то же самое воспроизводится локально.
 *
 * Значение `version` ниже — версия Kotlin DSL, и она не может быть новее сервера TeamCity.
 * Если сервер новее, её стоит поднять, чтобы стали доступны свежие возможности DSL.
 */

version = "2024.03"

project {
    description = "GlavLib: сборка пакетов NuGet в контейнере и их публикация"

    params {
        text("dotnet.sdk.image",
             "mcr.microsoft.com/dotnet/sdk:10.0.400",
             label = "Образ .NET SDK",
             description = "Образ, в котором собираются пакеты. Обязан нести Roslyn не старее " +
                           "версии Microsoft.CodeAnalysis.CSharp из Directory.Packages.props, " +
                           "иначе source-генератор не загрузится (ошибка CS9057)",
             allowEmpty = false)

        text("nuget.source",
             "https://api.nuget.org/v3/index.json",
             label = "Адрес NuGet-хранилища",
             description = "Куда публикуются собранные пакеты",
             allowEmpty = false)

        text("version.timezone",
             "",
             label = "Таймзона для даты в версии",
             description = "Идентификатор вида Europe/Moscow. Пустое значение означает " +
                           "таймзону агента",
             allowEmpty = true)

        password("nuget.api.key",
                 "",
                 label = "Ключ доступа к NuGet-хранилищу",
                 description = "Задаётся в настройках проекта TeamCity, в репозитории не хранится")
    }

    buildType(Pack)
    buildType(Publish)
}

object Pack : BuildType({
    name = "Pack"
    description = "Собирает решение в контейнере и упаковывает пакеты NuGet"

    artifactRules = "artifacts/*.nupkg"

    vcs {
        root(DslContext.settingsRoot)
    }

    params {
        // Версия считается из счётчика сборок, а не из её номера: номер сборки скрипт сам
        // заменяет на вычисленную версию, и брать его на вход означало бы считать версию
        // от версии предыдущей сборки.
        param("env.BUILD_COUNTER", "%build.counter%")
        param("env.BUILD_BRANCH", "%teamcity.build.branch%")
        param("env.CONFIGURATION", "Release")
        param("env.DOTNET_SDK_IMAGE", "%dotnet.sdk.image%")
        param("env.VERSION_TIMEZONE", "%version.timezone%")
    }

    steps {
        script {
            name = "Упаковать пакеты"
            scriptContent = "bash ci/pack.sh"
        }
    }

    triggers {
        vcs {
        }
    }

    requirements {
        exists("docker.version")
    }
})

object Publish : BuildType({
    name = "Publish"
    description = "Публикует пакеты, собранные конфигурацией Pack"

    vcs {
        root(DslContext.settingsRoot)
    }

    params {
        param("env.NUGET_SOURCE", "%nuget.source%")
        param("env.NUGET_API_KEY", "%nuget.api.key%")
    }

    steps {
        script {
            name = "Опубликовать пакеты"
            scriptContent = "bash ci/push.sh"
        }
    }

    dependencies {
        dependency(Pack) {
            snapshot {
                onDependencyFailure = FailureAction.FAIL_TO_START
            }

            artifacts {
                cleanDestination = true
                artifactRules = "*.nupkg => artifacts"
            }
        }
    }

    triggers {
        // Публикуется только основная ветка. Пакет из рабочей ветки собирается с предрелизным
        // суффиксом и при необходимости выкладывается запуском этой конфигурации вручную.
        finishBuildTrigger {
            buildType = "${Pack.id}"
            successfulOnly = true
            branchFilter = "+:<default>"
        }
    }
})
