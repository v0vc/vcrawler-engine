﻿--таблица с поддерживаемыми площадками
CREATE TABLE `credentials` (
`site` varchar(30) PRIMARY KEY NOT NULL,
`login` varchar(30) DEFAULT NULL,
`pass` varchar(30) DEFAULT NULL,
`cookie` varchar(255) DEFAULT NULL,
`passkey` varchar(255) DEFAULT NULL,
`autorization` INTEGER NOT NULL DEFAULT '0'
);

--заполняем
INSERT INTO "credentials" VALUES ('youtube.com', ' ', ' ', '', '', '0');
INSERT INTO "credentials" VALUES ('rutracker.org', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('tapochek.net', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('vimeo.com', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('nnm-club.me', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('novafilm.tv', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('livejournal.com', '', '', '', '', '0');
INSERT INTO "credentials" VALUES ('kinozal.tv', '', '', '', '', '0');

--таблица настроек
CREATE TABLE `settings` (
`key` varchar(30) PRIMARY KEY NOT NULL,
`val` varchar(255) NOT NULL
);

--заполняем
INSERT INTO "settings" VALUES ('pathToFfmpeg', '');
INSERT INTO "settings" VALUES ('pathToYoudl', '');
INSERT INTO "settings" VALUES ('pathToMpc', '');
INSERT INTO "settings" VALUES ('pathToDownload', '');
INSERT INTO "settings" VALUES ('youtubeBest', '0');

--таблица каналов
CREATE TABLE `channels` (
`id` varchar(30) PRIMARY KEY NOT NULL,
`title` varchar(255) NOT NULL,
`subtitle` varchar(255) NULL,
`thumbnail` BLOB  NULL,
`site` varchar(30) NOT NULL,
FOREIGN KEY(`site`) REFERENCES credentials(`site`) ON DELETE CASCADE
);

--таблица загруженных элементов
CREATE TABLE `items` (
`id` varchar(30) PRIMARY KEY NOT NULL,
`parentid` varchar(30),
`title` varchar(250) NOT NULL DEFAULT '',
`description` varchar(250) NOT NULL ,
`viewcount` INTEGER NOT NULL DEFAULT '0',
`duration` INTEGER NOT NULL DEFAULT '0',
`comments` INTEGER NOT NULL DEFAULT '0',
`thumbnail` BLOB  NULL,
`timestamp` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
FOREIGN KEY(`parentid`) REFERENCES channels(`id`) ON DELETE CASCADE
);

--таблица тэгов
CREATE TABLE `tags` (
`title` varchar(30) PRIMARY KEY NOT NULL
);

--заполняем первоначальный набор тэгов
INSERT INTO "tags" VALUES ('авто');
INSERT INTO "tags" VALUES ('мото');
INSERT INTO "tags" VALUES ('обзоры');
INSERT INTO "tags" VALUES ('новости');
INSERT INTO "tags" VALUES ('игры');
INSERT INTO "tags" VALUES ('торрент');
INSERT INTO "tags" VALUES ('смешное');
INSERT INTO "tags" VALUES ('аудио');
INSERT INTO "tags" VALUES ('видео');
INSERT INTO "tags" VALUES ('фильмы');
INSERT INTO "tags" VALUES ('техника');

--таблица соответвий тэгов каналам
CREATE TABLE `channeltags` (
`channelid` varchar(30) NOT NULL,
`tagid` varchar(30) NOT NULL,
FOREIGN KEY(`channelid`) REFERENCES channels(`id`) ON DELETE CASCADE,
FOREIGN KEY(`tagid`) REFERENCES tags(`title`) ON DELETE CASCADE,
PRIMARY KEY(`channelid`,`tagid`)
);

--таблица плэйлистов
CREATE TABLE `playlists` (
`id` varchar(30) PRIMARY KEY NOT NULL,
`channelid` varchar(30) NOT NULL,
`title` varchar(150) NOT NULL,
`subtitle` varchar(255),
`thumbnail` BLOB  NULL,
FOREIGN KEY(`channelid`) REFERENCES channels(`id`) ON DELETE CASCADE
);

--таблица соответвий пэейлистов элементам
CREATE TABLE `playlistitems` (
`playlistid` varchar(30) NOT NULL,
`itemid` varchar(30) NOT NULL,
`channelid` varchar(30) NOT NULL,
FOREIGN KEY(`playlistid`) REFERENCES playlists(`id`) ON DELETE CASCADE,
FOREIGN KEY(`itemid`) REFERENCES items(`id`) ON DELETE CASCADE,
FOREIGN KEY(`channelid`) REFERENCES channels(`id`) ON DELETE CASCADE,
PRIMARY KEY (playlistid, itemid)
);