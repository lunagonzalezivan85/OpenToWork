---
description: Steps to migrate the OpenToWork postulant portal from Blazor Server to Blazor WebAssembly
---
# Migrate Postulant Portal to Blazor WebAssembly

Use this skill when migrating `OpenToWork.WEB` from Blazor Server to Blazor WebAssembly. Keep `OpenToWork.AdminWEB` on Interactive Server.

## Steps

1. Create a new project `OpenToWork.WEB.Client` (Blazor WebAssembly).
2. Move `.razor` components from `OpenToWork.WEB` to `OpenToWork.WEB.Client`.
3. Replace `AddInteractiveServerComponents` with `AddInteractiveWebAssemblyComponents`.
4. Move services that use `HttpClient` to the client project.
5. Configure the render mode for the migrated components.
6. Keep `OpenToWork.AdminWEB` as Blazor Server (Interactive Server).
