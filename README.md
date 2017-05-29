# Mongolian Barbecue

It's just a message queue that (ab)uses MongoDB to do its thing.

## Example

Let's say we have a MongoDB instance running on `MONGOBONGO01`, and we want to use the `Readme` database for exchanging some messages.

The MongoDB connection string looks like this: `mongodb://MONGOBONGO01/Readme`, so we simply

	var config = new Config("mongodb://MONGOBONGO01/Readme", "messages");

to create a configuration that uses the `messages` collection for exchanging messages.

## How to send messages?

Grab the configuration from before and get a producer from it:

	var producer = config.CreateProducer();

and then send a byte array payload to `queue-a` like this:

	var payload = new byte[] { 0xC0, 0xFF, 0x33, 0xBA, 0xDA, 0x55 };

	await producer.SendAsync("queue-a", new Message(payload));

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