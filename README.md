# Masstransit-reconnection-reproduction

To restore packages, run `.paket/paket.exe restore` or `./paket.sh restore`.

# To reproduce
Run both console apps at the same time. From the client, do a couple 
of publishes or requests to check that everything works, then shutdown the rabbit 
node that they are connected to.

At this point we get messages in the console saying one of either
* `Rabbitmq Connect Failed: Operation interrupted`
* `RabbitMQ Connect Failed: Broker unreachable: guest@localhost:5672/`

Which message we get is depending on how we have connected to rabbit.

At this point we start the rabbit node again and wait for it to get up. 
Then we start publishing things from the client again. These messages seem to get 
published (although we may get one or two exceptions, but usually the client seem to have
reconnected properly after the first publish attempt). 

However the consumer never reconnects unless we restart that console app.
