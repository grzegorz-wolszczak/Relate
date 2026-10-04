# RelateMaui

## Language: English only

All text that lives in this repository must be written in **English**:

- **Code comments** (`//`, `/* */`, `///`, XML/MSBuild comments in `.csproj` / `.proj`).
- **User-facing strings in the app** — dialog titles/messages, button labels,
  toasts, action-sheet options, page/section headers, placeholders, error and
  log messages.
- **All other repo content** — Markdown docs, `README`, `notes.txt`, plan files,
  commit messages, and pull request descriptions.

**Only exception:** strings that are resolved from the device's system language
settings on the user's phone must stay as-is (not hardcoded, not translated).
In this project that means values coming from the OS, e.g.
`ContactsContract.CommonDataKinds.Phone.GetTypeLabel(...)`,
`CallLog.Calls.CachedName`, contact `DisplayName`, and the native Android
permission dialogs. A hardcoded fallback next to such a call (e.g. `?? "Other"`)
is still our code and stays English.

There is no localization layer (`.resx` / `IStringLocalizer`) — UI strings are
hardcoded literals, so translate/write them in place.
