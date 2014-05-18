ENode is a framework aims to help us developing ddd, cqrs, eda, and event sourcing style applications.

Developing rules:
--------------
- One command only allowed to effect one aggregate
- Strong consistency in one aggregate
- Eventual consistency between multiple aggregates
- Unit of Work pattern is not need again, replaced with saga
- Domain event is the only way to implement aggregate interaction

Saga sample
--------------
https://github.com/tangxuehua/BankTransferSample

A simple forum sample
--------------
https://github.com/tangxuehua/forum

architecture
--------------
![alt tag](https://raw.githubusercontent.com/tangxuehua/enode/master/doc/enode%20arch.png)