# Security Policy

## Reporting a vulnerability

Please do not publish exploit details or sensitive logs in a public issue. Use GitHub's private vulnerability reporting feature if it is enabled for this repository. Otherwise, contact the repository maintainer privately before disclosing details.

## Safety-sensitive behavior

Velvet Pointer installs a low-level mouse hook and injects replacement movement while enhancement is enabled. Unexpected pointer behavior should be treated as a reliability issue. The emergency bypass is **Ctrl + Alt + Pause**; exiting the app removes its hook.

Do not use this experimental prototype in safety-critical environments.
