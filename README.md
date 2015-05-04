ENode is a framework aims to help us developing ddd, cqrs, eda, and event sourcing style applications.

Developing rules:
--------------
- One command only allowed to effect one aggregate
- Strong consistency in one aggregate
- Eventual consistency between multiple aggregates
- Unit of Work pattern is not need again, replaced with saga
- Domain event is the only way to implement aggregate interaction

Blog
--------------
http://www.cnblogs.com/netfocus/category/496012.html

A simple forum sample
--------------
https://github.com/tangxuehua/forum

A conference management and registration sample
--------------
https://github.com/tangxuehua/conference

architecture
--------------
![alt tag](https://raw.githubusercontent.com/tangxuehua/enode/master/doc/arch.png)
