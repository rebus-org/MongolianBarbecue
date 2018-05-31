# Mongolian Barbecue

It's just a message queue implementation that (ab)uses MongoDB to do its thing :eyes:

Another way to put it is: This library lets you pretend that MongoDB is a message queue :speak_no_evil:

## Example

Let's say we have a MongoDB instance running on `MONGOBONGO01`, and we want to use the `Readme` database for exchanging some messages.

The MongoDB connection string looks like this: `mongodb://MONGOBONGO01/Readme`, so we simply

	var config = new Config("mongodb://MONGOBONGO01/Readme", "messages");

to create a configuration that uses the `messages` collection for exchanging messages.

:+1:



## How to send messages?

Grab the configuration from before and get a producer from it:

	var producer = config.CreateProducer();

and then send a byte array payload to `queue-a` like this:

	var payload = new byte[] { 0xC0, 0xFF, 0x33, 0xBA, 0xDA, 0x55 };

	await producer.SendAsync("queue-a", new Message(payload));

:clap:



## How to receive messages?

Go back to the configuration from before and get a consumer from it:

	var consumer = config.CreateConsumer("queue-a");

and then receive the next message (or null if none was to be found) like this:

	var message = await consumer.GetNextAsync();

	if (message != null) 
	{
		// we got a message - handle it here:

		try
		{
			await HandleItSomehow(message);
			
			// acknowledge it (i.e. delete the message)
			await message.Ack();
		}
		catch(Exception exception) 
		{
			// try to return message immediately (don't worry if this fails - the lease will eventually expire)
			await message.Nack();

			throw;
		}
	}

:ok_hand:



## How to configure things?

The constructor of the configuration object accepts a couple of optional parameters, allowing you to customize a couple of things.

### Message lease timeout

By default, a message is made invisible for 60 seconds when it is received. If it is ACKed within that time, it is removed from the queue
(i.e. it is deleted), but if that does not happen - e.g. if the consumer crashes & burns in a haze of `OutOfMemoryException`s and 
`StackOverflowExceptions`s - then the message will automatically become visible to other consumers again, once the lease expires.

If 60 seconds is not what you want, you can customize it like this:

    var config = new Config(..., ..., defaultMessageLeaseSeconds: 20);

to lower the lease timeout to 20 seconds.

### Max parallelism

The MongoDB driver does not seem to protect itself from connection pool depletion resulting from too many concurrent asynchronous
operations, so we may limit the number of concurrent operations per `Consumer` / `Producer` object instance by passing a value
to the configuration object like this:

    var config = new Config(..., ..., maxParallelism: 10);

The default value for "max parallelism" is 20.
