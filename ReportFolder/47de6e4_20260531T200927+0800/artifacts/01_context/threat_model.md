# StoryBook2 Repository Threat Model

## Overview

StoryBook2 is a single ASP.NET Core Razor Pages web application that serves a bilingual, child-friendly nature storybook. The runtime product lives primarily under `StoryBook/`; `StoryBook.Tests/`, `specs/`, `docs/`, and `markdownFolder/` are development and specification assets. The application renders server-side Razor Pages, reads local JSON catalogs from `StoryBook/Data/`, serves static images and first-party JavaScript/CSS from `StoryBook/wwwroot/`, and uses Bootstrap/jQuery assets vendored under `StoryBook/wwwroot/lib/`.

The public runtime routes include `/`, `/Privacy`, `/Error`, `/dinosaurs`, `/dinosaurs/{slug}`, `/aquarium`, `/aquarium/{slug}`, `/explore`, `/compare`, `/journeys`, `/journeys/{slug}`, `/passport`, and `/quiz`. The application does not define login, accounts, database access, external API calls, file uploads, administrative mutation endpoints, or server-side persistence of user choices. Most user interactions are read-only navigation, search/filtering, language/theme selection, image modal behavior, comparison, quiz answer submission, and local reading-passport state.

Primary assets and invariants:

- Story content, quiz data, route slugs, image paths, and bilingual text should remain controlled by repository-maintained JSON, not runtime users.
- Server-rendered pages should HTML-encode content and avoid unsafe script or markup injection.
- Catalog file loading should stay limited to intended content files and should not become attacker-controlled path access.
- Friendly error states should avoid leaking absolute paths, stack traces, secrets, or implementation details.
- Client-side localStorage state should not be treated as a trusted security boundary.

## Threat Model, Trust Boundaries, and Assumptions

Trust boundaries:

- Public browser to Razor Pages: request paths, route values, query strings, form fields, headers, and cookies are attacker-controlled.
- Server app to local catalog files: `ContentPath` values are operator/developer-controlled configuration, not public user input.
- Repository content to rendered HTML: JSON catalog text, alt text, slugs, and image paths are developer-controlled but still cross into HTML, attributes, and JavaScript-enhanced UI.
- Browser storage to client scripts: `localStorage` keys such as `storybook.language`, `storybook.theme`, and reading passport state are fully user-controlled and must be validated or safely ignored.
- Static assets and vendored libraries: first-party scripts are part of the app; third-party vendored assets carry dependency and supply-chain risk but are not dynamically fetched at runtime.

Assumptions:

- The production deployment terminates HTTPS normally and honors ASP.NET Core HTTPS redirection/HSTS behavior when not in Development.
- No authentication or authorization boundary protects storybook content today; public read access is an intended product property.
- JSON catalog files are not writable by remote users in normal deployment.
- `appsettings*.json`, project files, and static assets do not contain production secrets.
- Tests can create temporary files, but test-only file writes are not runtime attack surface unless the same behavior exists in `StoryBook/`.

## Attack Surface, Mitigations, and Attacker Stories

Relevant attack surfaces:

- Razor route handlers and PageModels that bind route/query/form values, especially detail slugs, search terms, quiz scope/question identifiers, and quiz answer form fields.
- Catalog services that read JSON via `File.ReadAllText`, deserialize using `System.Text.Json`, cache snapshots, and resolve configured `ContentPath` values.
- Razor templates that render catalog text, image paths, hrefs, data attributes, and script/CSS includes.
- First-party JavaScript that reads/writes localStorage, manipulates DOM state, opens image modal views, and applies language/theme/passport state.
- Static files under `wwwroot`, including images, first-party scripts/styles, and vendored dependency files.
- Error and fallback pages that could accidentally expose internal details.
- Dependency versions in `.csproj` files and vendored JavaScript/CSS libraries.

Existing mitigations visible in repository structure:

- Razor Pages and Tag Helpers provide HTML and attribute encoding by default when rendering normal model values.
- Content loading is based on local files under application content root by default, not request-supplied file paths.
- Services and validators enforce slug, image path, bilingual text, content length, and fallback rules in several catalog areas.
- Public user choices are stored client-side for convenience; server-side correctness and catalog loading do not depend on trusting localStorage.
- Tests include route, fallback, script-contract, and content-validation coverage for many feature boundaries.
- `Program.cs` enables HTTPS redirection and HSTS outside Development.

Realistic attacker stories:

- A remote unauthenticated visitor manipulates route slugs, query parameters, or quiz form fields to trigger unexpected server behavior, information disclosure, or unsafe rendering.
- A remote visitor tampers with localStorage values to test client-side trust assumptions, DOM injection paths, or state confusion.
- A malicious or compromised content contributor commits unsafe JSON content that later renders into HTML, attributes, links, or client-side state.
- An operator misconfigures catalog `ContentPath` to an unintended absolute path or sensitive file and then exposes data through runtime rendering or logs.
- A dependency or vendored static library contains a known vulnerability relevant to how the app uses it.

Less realistic or out-of-scope stories for the current repository:

- Cross-tenant authorization bypass: no tenant, account, role, or private object model exists.
- SQL/NoSQL/LDAP injection: no database or query engine exists.
- SSRF: no server-side outbound HTTP client or URL fetch feature exists.
- Remote code execution through upload/archive extraction: no upload, extraction, or dynamic code execution feature exists.
- Credential theft from server-side storage: no production credentials are expected in source or config.

## Severity Calibration (Critical, High, Medium, Low)

Critical findings would require a remote path to arbitrary code execution, arbitrary server file read/write across a sensitive boundary, authentication bypass for a newly protected control plane, or secret exposure that directly grants production control. Examples would include attacker-controlled file path traversal into sensitive files, unsafe deserialization of public input, or a deployed secret committed to runtime config.

High findings would require meaningful compromise of integrity or confidentiality across the public web boundary, such as stored or reflected XSS reachable through normal browsing, public access to sensitive non-story data, unsafe catalog path selection reachable by public users, or a dependency vulnerability with a concrete exploitable path in this app.

Medium findings would include security-relevant weaknesses with narrower impact or stronger preconditions, such as mis-scoped CSRF on a non-sensitive state change, error disclosure that reveals internal paths without secrets, unsafe handling of operator-controlled content paths, or client-side state manipulation that causes misleading but non-privileged behavior.

Low findings would include hardening gaps with limited security impact in this app context, such as missing defensive headers, benign localStorage tampering, outdated dependencies without a relevant reachable feature, or robustness issues that only affect public, non-sensitive content display.
