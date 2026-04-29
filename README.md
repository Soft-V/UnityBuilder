<p align="center">
    <img width="1012" height="540" alt="horizontal_banner" src="https://github.com/user-attachments/assets/1ccab210-b61e-41d2-aac4-03eea8cd0e54" />
</p>

**A simple desktop app to automate Unity project builds across multiple platforms**

*Tired of switching platforms manually in Unity and dropping builds where they need to go? Same.*

---

## Features

- **Multi-platform builds** — Windows (x64/x86/ARM64), Linux (x64), macOS (Universal)
- **Visual pipeline** — node graph with real-time progress, status and timers for each step
- **FTP upload** — automatically send builds to your own server after a successful build
- **Change indexing** — integration with [Hash Computer](https://github.com/CrackAndDie/HashComputer) for incremental updates
- **Auto-update** — new versions are downloaded straight from GitHub Releases via an in-app button

---

## Quick Start

### Requirements

- **Unity** installed with build support modules for your target platforms:
  - `Windows Build Support`
  - `Linux Build Support`
  - `Mac Build Support`

### Usage

1. Set the path to your Unity executable
2. Select your project folder
3. Set the Unity version, output directory and project name
4. Choose target platforms
5. *(Optional)* configure FTP upload and hash indexing
6. Hit **Start** and watch the pipeline run

---

## Supported Platforms

| Platform | Architectures | Note |
|---|---|---|
| Windows | x64, x86, ARM64 | |
| Linux | x64 | ARM64 not supported in Unity Community |
| macOS | Universal | Single build for both Silicon and Intel |
| Android | — | Planned — PRs welcome |

---

## How the Pipeline Works

Once a build is started, you see a **visual node graph** where every step is represented as a card.

**Concurrency rules:**

| Process | Parallelism |
|---|---|
| Build (Unity) | strictly 1 — interacts with Unity directly |
| Hash Computer | up to 10 at once |
| FTP Upload | up to 10 at once |

**Each node card shows:**
- Current status
- Execution progress
- Step execution time
- Total pipeline time

---

## Contributing

The project was built for our own needs, but we'd love any contribution!

- Found a bug — [open an Issue](https://github.com/Soft-V/UnityBuilder/issues/new)
- Have an idea — also in Issues, let's discuss
- Want to write code — PRs are very welcome
- Just drop a star — it matters too

On the roadmap: a **CLI version** for easier server deployment. If you want to help — you know what to do.

---

## Screenshots

<img width="1215" height="743" alt="Снимок экрана — 2026-04-29 в 21 20 19" src="https://github.com/user-attachments/assets/84bddc9c-283a-418c-86c7-515536b30735" />

<img width="1215" height="743" alt="Снимок экрана — 2026-04-29 в 21 20 37" src="https://github.com/user-attachments/assets/b43cf753-d46f-4d2d-a5af-59121c35260c" />

<img width="1215" height="743" alt="Снимок экрана — 2026-04-29 в 21 20 57" src="https://github.com/user-attachments/assets/1c999058-bad0-485f-ab74-c78b1f277274" />

<img width="1215" height="743" alt="Снимок экрана — 2026-04-29 в 21 21 17" src="https://github.com/user-attachments/assets/13a290c9-329b-415d-91ef-d39a2e202faf" />


---

<div align="center">

Made with ❤️ for everyone tired of clicking through platforms by hand

</div>
