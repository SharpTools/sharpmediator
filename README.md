# sharpmediator
Cool Mediator implementation using weak references

**Nuget: sharpmediator**

## Super easy to use:

### Subscribe to some message type

```
   var mediator = new Mediator();
   mediator.Subscrive<SomeType>(this, SomeMethod);
```

### Publish!

All subscribers will be called:
```
   mediator.Publish(new SomeType());
```

### Unsubscribe

Don't want to receive any more messages? Just unsubscribe :)
```
   mediator.Unsubscribe(this);
```

