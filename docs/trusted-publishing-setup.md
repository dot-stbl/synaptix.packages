# NuGet Trusted Publishing — manual setup

## Текущее состояние

- ✅ Tag `v-analyzers-1.1.0` запушен
- ✅ Build + pack + GitHub Release работают (см. https://github.com/dot-stbl/synaptix.packages/releases/tag/v-analyzers-1.1.0)
- ❌ NuGet push упал: `Token exchange failed (HTTP 400): The organization 'dot-stbl' does not have a confirmed email address`

## Шаги для исправления (делаются ВРУЧНУЮ на nuget.org)

### 1. Зайти на https://www.nuget.org/account/TrustedPublishing

### 2. Удалить старую policy
- Найти policy с owner=`dot-stbl` (та что мы создали раньше)
- Нажать Delete / Remove
- Confirm удаление

### 3. Создать новую policy с правильным owner
- Нажать "Add new policy" (или аналогичная кнопка)
- Заполнить:
  - **Owner:** `lowern1ght` ← **важно: личный аккаунт, не org**
  - **Repository:** `synaptix.packages`
  - **Workflow file:** `release.yml`
  - **Environment:** `nuget-publish`
- Нажать Create / Save

### 4. Проверить email подтверждение
- Зайти в https://www.nuget.org/account (Account settings)
- Убедиться что у `lowern1ght` подтверждён email
  - Если нет — нажать "Send confirmation email" и пройти по ссылке

## Re-run workflow

После изменения policy:

```bash
gh run rerun 30595040404 --failed-only
```

Или через GitHub UI: https://github.com/dot-stbl/synaptix.packages/actions/runs/30595040404 → Re-run jobs

## Что должно произойти

- NuGet/login@v1 успешно обменяет OIDC токен
- `dotnet nuget push` зальёт nupkg на https://www.nuget.org/packages/Synaptix.Analyzers
- `Synaptix.Analyzers 1.0.0` появится в NuGet
- `Synaptix.Text.Case 1.0.0` уже там (legacy, не трогаем)
- После успешного push владелец policy сможет сменить обратно на `dot-stbl` если хочется

## Если что-то пошло не так

Если после re-run ошибка изменилась — пришлите мне новый лог. Типичные проблемы:
- 401 Unauthorized → username в workflow не совпадает с policy creator (должен быть `lowern1ght`)
- Email not confirmed → проверьте https://www.nuget.org/account что email верифицирован
- Repository not found → проверьте что policy owner имеет доступ к `dot-stbl/synaptix.packages`
