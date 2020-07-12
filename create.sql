-- we don't know how to generate root <with-no-name> (class Root) :(
create table Cart
(
	Id INTEGER not null
		primary key autoincrement,
	Status INTEGER default 0 not null
);

create table Product
(
	Sku VARCHAR(32) not null
		primary key,
	Name varchar(255) not null,
	Price INTEGER not null
);

create table CartProduct
(
	CartId INTEGER not null
		references Cart
			on update restrict on delete restrict,
	Sku VARCHAR(32) not null
		references Product
			on update restrict on delete restrict,
	primary key (CartId, Sku)
);

