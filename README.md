A framework aims to help us developing ddd, cqrs, eda, and event sourcing style applications.

Rules:
=====
- 1. One command only effect one aggregate;
- 2. Strong consistency for one aggregate;
- 3. Eventual consistency for multiple aggregates;
- 4. Unit of Work pattern is not need again, replaced with saga;
- 5. Domain event is the only way to implement aggregate interaction;

Saga sample
=====
https://github.com/tangxuehua/BankTransferSample

A simple forum sample
=====
https://github.com/tangxuehua/forum

architecture
=====
![alt tag](https://raw.githubusercontent.com/tangxuehua/enode/master/doc/enode%20arch.png)