# Encryphix - Advanced File & Folder Encryption Software

[![GitHub downloads](https://img.shields.io/github/downloads/turkaysoft/encryphix/total?style=flat&color=1a893c&label=Downloads)](https://github.com/turkaysoft/encryphix/releases)
[![GitHub stars](https://img.shields.io/github/stars/turkaysoft/encryphix?style=flat&color=0062cc&label=Stars)](https://github.com/turkaysoft/encryphix/stargazers)
[![GitHub release](https://img.shields.io/github/v/release/turkaysoft/encryphix?style=flat&color=5a32a3&label=Latest%20Release)](https://github.com/turkaysoft/encryphix/releases/latest)
[![Platform](https://img.shields.io/badge/platform-Windows-b31d28?style=flat&label=Platform)](https://github.com/turkaysoft/encryphix)

**Encryphix** is a high-security **File and Folder Encryption Software** developed by **Eray Türkay**. It provides professional-grade protection for your sensitive data using industry-standard **PBKDF2** and **AES-256** encryption algorithms. Designed for users who demand both security and simplicity, Encryphix ensures your private files remain truly private.

---

### Donate
You can support this project by making a donation to help ensure its sustainability and the development of new features.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-Donate-0a6628?style=flat&logo=buy-me-a-coffee&logoColor=white)](https://buymeacoffee.com/turkaysoft)

---

## Key Features

* **Military-Grade Security:** Utilizes **AES-256** and **PBKDF2** for robust data encryption.
* **HMAC-SHA256 Integrity Verification:** Encrypted files include an integrity verification system. Any unauthorized modification to the file content is instantly detected and reported as "CorruptedFileOrTampered", protecting your data from undetected tampering.
* **Secure File Deletion:** Before permanent deletion, files are overwritten with cryptographically random data using multiple passes, making forensic recovery impossible and ensuring deleted data cannot be reconstructed.
* **Path Traversal Protection:** A file name sanitization mechanism actively blocks path traversal attacks, preventing malicious actors from accessing files outside the intended directory structure.
* **Cryptographically Secure Password Generator:** Uses `RandomNumberGenerator`, implements Rejection Sampling to eliminate modulo bias, and applies Fisher-Yates Shuffle for secure character randomization. Each generated password guarantees at least one uppercase letter, one lowercase letter, one digit, and one symbol, with a random length between 10-18 characters.
* **Auto-Clipboard Clear:** Copied passwords are automatically removed from the clipboard after 30 seconds. If no new data is copied within this period, the clipboard is cleared — preventing accidental password leaks through paste operations.
* **Separate Key Derivation:** The key derivation system generates independent 32-byte AES encryption keys and 32-byte HMAC integrity keys, providing stronger cryptographic separation between encryption and verification operations.
* **Pure Performance:** Developed exclusively in **C# and .NET Framework** with no external libraries or dependencies.
* **Modern UI:** Sophisticated interface with Light, Dark, and System theme support.
* **Multilingual:** It supports 15 different languages, primarily English. You can access the supported languages here: [Supported Languages](https://github.com/turkaysoft/encryphix/discussions/1)
* **Portable:** No installation required. Carry your encryption tool anywhere and run it instantly.
* **Completely Free:** High-end encryption features provided at no cost to the user.
* **Built-in Update Mechanism:** It features a built-in smart update mechanism developed specifically by **Türkaysoft**.

---

## Interface Preview

<img width="1010" height="633" alt="Encryphix UI" src="https://github.com/user-attachments/assets/313cdcf0-909c-4ef7-a64a-431700eee42b" />

## Password Generator

<img width="586" height="533" alt="Encryphix Password Manager" src="https://github.com/user-attachments/assets/8738b7a6-68b1-43f8-9779-cc63ee0a5212" />

---

## Advanced Capabilities

Encryphix combines security with practical utility to offer a superior encryption experience:

* **Secure Compression:** Optionally compress your data while encrypting it to save storage space.
* **Folder & File Support:** Encrypt individual files or entire directory structures with equal ease.
* **Multi-Architecture Support:** Fully compatible with both x64-x86 and ARM processor architectures.

---

## Translation Support

* **Translation Support:** Community-driven localization via the official [Translation Guide](https://github.com/turkaysoft/encryphix/discussions/1).

---

## System Requirements

| Feature | Minimum Requirements | Recommended Requirements |
| :--- | :--- | :--- |
| **OS** | Windows 10 20H2 x64 | Windows 10 22H2 x64 |
| **CPU** | x64 or ARM64 | x64 or ARM64 |
| **RAM** | 50 MB Free RAM | 75 MB Free RAM |
| **.NET** | .NET Framework 4.8.1 | .NET Framework 4.8.1 |

---

## Getting Started

1.  Navigate to the **[Releases](https://github.com/turkaysoft/encryphix/releases/latest)** page.
2.  Download the latest ZIP file.
3.  **Extract all files from the ZIP** (Important: Application requires all folder contents to run correctly).
4.  Launch the executable corresponding to your architecture:
    * `Encryphix_x64.exe`: For standard 64-bit Intel/AMD systems.
    * `Encryphix_arm64.exe`: For ARM-based devices like Surface Pro.

---

## Security

* **Zero Data Export Policy:** Your privacy is our priority; no data leaves your machine.
* **No Dependencies:** Developed entirely from scratch using its own source code, there are no risks from security vulnerabilities in third-party libraries.
* **Open Source:** All source code for the program is open and can be reviewed by anyone.

---

## License

This software is offered free of charge as part of the **Türkaysoft solutions package** and is protected under the [**MIT License**](https://github.com/turkaysoft/encryphix?tab=MIT-1-ov-file).
