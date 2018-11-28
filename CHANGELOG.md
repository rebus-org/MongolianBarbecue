# Changelog

## 1.0.0

* Simple single-collection "message queue" implemented with MongoDB's find-and-modify operations
* Lease-based locking of received messages
* Configurable locking times
* Proper duplicate message ID exception in the form of `UniqueMessageIdViolationException`
* Add ability to renew lease for a message
* Additional config ctor overload
* Add ability to load single message by its ID (e.g. for when it has failed too many times)