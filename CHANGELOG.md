# Changelog

## 1.0.0
* Simple single-collection "message queue" implemented with MongoDB's find-and-modify operations
* Lease-based locking of received messages
* Configurable locking times
* Proper duplicate message ID exception in the form of `UniqueMessageIdViolationException`
* Add ability to renew lease for a message
* Additional config ctor overload
* Add ability to load single message by its ID (e.g. for when it has failed too many times)

## 2.0.0
* Update packages
* Only target .NET Standard 2.0
* Use `AsyncSemaphore` from Nito.Asyncex instead of `SemaphoreSlim`

## 2.1.0
* Update packages

## 3.0.0
* Update to v3 of MongoDB.Driver