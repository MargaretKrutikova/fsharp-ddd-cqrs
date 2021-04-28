# Example project with DDD, CQRD in an onion

This is a tiny api that implements a simple peer-to-peer library for exchanging physical books.

Wait, you might say, that doesn't make sense. Well, listen, you have a book, I have a book. I have read my book, you have read yours. My book is on the shelf gathering dust, your book is now a monitor stand. I want to read your book, would you mind? Maybe you want to read mine. Let's do it for other books and keep track of those we borrow from each other. And maybe Bob and Alice want to join and share their books too?

## Is it serious?

No, this is a small prototype of how one could go about developing a microservice using **D**omain-**D**riven-**D**esign combined with **C**ommand-**Q**uery-**R**esponsibility-**S**egregation pattern using the onion architecture.

There is a presentation accompanying this project on [slides.com](https://slides.com/margaretkru/deck-793155).

## Technologies

The project is developed with `F#`, which is a great choice for domain modelling and just in general a really **amazing** language. The api is built in `.NET 5` with [`Giraffe`](https://giraffe.wiki/).

## More yet to come
