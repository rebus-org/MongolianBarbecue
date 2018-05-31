# Changelog

## 1.0.0-b11

* Simple single-collection "message queue" implemented with MongoDB's find-and-modify operations
* Lease-based locking of received messages
* Configurable locking times
* Proper duplicate message ID exception in the form of `UniqueMessageIdViolationException`
* Add ability to renew lease for a message