# SecuViewer

This is my hobby multiplatform text file encryption system.

## Purpose

To store passwords in a text file which is impossible to open without a password - and whih will survive WIndows reinstalls, applications reinstalls, HDD crashes etc.

Also, to be able to open such a file on both desktop and laptop.

Also, to ensure my data is not available to 3rd-partys in unencrypted form (read: no password managers).

## Implementation

The solution consists of:

* Windows application (Win32 - WinForms), capable of opening plain or encrypted file, and save an encrypted one.
* Android app capable of decrypting the file.
* The Cryption module implementing custom DES-alike algorythm based entirely on user's password and ensuring there is no fast way of knowing whether the password is right or wrong.
* Delivery engine, at the moment it's OneDrive.

## Intention

The solution is intended for private usage and is not published to Play Store or Windows Store.

## Privacy

Both applications use no cookies, store no user data (except explicitly requested by user to be saved), and do not work with network at all.

## Versioning

The Crypter infrastructure features backwards-compatible versioned Cryptors, which automatically define the most applicable Cryptor for decryption, and use the latest one for encryption.
That is, even if you encrypted a file with earlier version of the app, you will be able to decrypt it with the latest one.

## Hashing algorithm

The Deterministic Hashing Algorithm and the reasoning for it's use is described [here](https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/) and is written by Andrew Lock.

## Usage of this tool

You can use the tool under the terms of UAYOR (use at your own risk) conditions. Works on my machine (tm).
