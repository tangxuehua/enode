CREATE DATABASE `ENode`;

USE `ENode`;

CREATE TABLE `EventStream` (
  `Sequence` bigint(20) NOT NULL AUTO_INCREMENT,
  `AggregateRootTypeName` varchar(256) NOT NULL,
  `AggregateRootId` varchar(36) NOT NULL,
  `Version` int(11) NOT NULL,
  `CommandId` varchar(36) NOT NULL,
  `CreatedOn` datetime NOT NULL,
  `Events` varchar(4000) NOT NULL,
  PRIMARY KEY (`Sequence`),
  UNIQUE KEY `IX_EventStream_AggId_Version` (`AggregateRootId`,`Version`),
  UNIQUE KEY `IX_EventStream_AggId_CommandId` (`AggregateRootId`,`CommandId`)
) ENGINE=InnoDB AUTO_INCREMENT=130 DEFAULT CHARSET=utf8;

CREATE TABLE `PublishedVersion` (
  `Sequence` bigint(20) NOT NULL AUTO_INCREMENT,
  `ProcessorName` varchar(128) NOT NULL,
  `AggregateRootTypeName` varchar(256) NOT NULL,
  `AggregateRootId` varchar(36) NOT NULL,
  `Version` int(11) NOT NULL,
  `CreatedOn` datetime NOT NULL,
  `UpdatedOn` datetime NOT NULL,
  PRIMARY KEY (`Sequence`),
  UNIQUE KEY `IX_PublishedVersion_AggId_Version` (`ProcessorName`,`AggregateRootId`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8;

CREATE TABLE `LockKey` (
  `Name` varchar(128) NOT NULL,
  PRIMARY KEY (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
