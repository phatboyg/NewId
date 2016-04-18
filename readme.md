## What is it
NewId can be used as an embedded unique ID generator that produces 128 bit (16 bytes) sequential IDs. It is inspired from snowflake and flake. Read on to learn more.

### The Problem

A number of applications use unique identifiers to identify a data record. A common way for apps that use a relational database (RDBMS) is to delegate the generation of these IDs to the database - by means of a Identity column (MS-SQL) or similar.
This approach is fine for a small app, but quickly becomes a bottleneck at web-scale. See this post from the blokes at twitter: https://blog.twitter.com/2010/announcing-snowflake 
Another use case is apps that use messaging to communicate between themselves - as is the case with a Microservices based architecture. These apps may require sequential unique IDs for messages.

### An attempt at solutions
A trivial approach is to use GUIDs/UUIDs generated in applications. While that works, in most frameworks GUIDs are not sequential. This takes away the ability to sort records based on their unique ids.

### The Solution
The Erlang library flake (https://github.com/boundary/flake) adopted an approach of generating 128-bit, k-ordered ids (read time-ordered lexically) using the machines MAC, timestamp and a per thread sequence number. These IDs are sequential and wouldn't collide in a cluster of nodes running applicaitons that use these as UUIDs.

### Sample Code

```csharp
NewId id = NewId.Next(); //produces an id like {11790000-cf25-b808-dc58-08d367322210}

// Supports operations similar to GUID
NewId id = NewId.Next().ToString("D").ToUpperInvariant();
// Produces 11790000-CF25-B808-2365-08D36732603A

// Start from an id
NewId id = new NewId("11790000-cf25-b808-dc58-08d367322210");

// Start with a byte-array
var bytes = new byte[] { 16, 23, 54, 74, 21, 14, 75, 32, 44, 41, 31, 10, 11, 12, 86, 42 };
NewId theId = new NewId(bytes);
```
### When NOT to use sequential IDs
(Adapted from the flake readme)
The generated ids are predictable by design. They should not be used in scenarios where unpredictability is a desired feature. 
These IDs should **NOT** be used for:
- Generating passwords
- Security tokens 
- Anything else you wouldn't want someone to be able to guess.

NewId generated ids expose the identity of the machine which generated the id (by way of its MAC address) and the time at which it did so. This could be a problem for some security-sensitive applications.

**Don't** do modulo 2 arithmetic on flake ids with the expectation of random distribution. 