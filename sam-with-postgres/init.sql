CREATE TABLE hello_table (
                             id serial NOT NULL PRIMARY KEY,
                             hello_source varchar(20),
                             hello_target varchar(20)
);

CREATE TABLE product_photo (
    image_uri varchar(128) NOT NULL PRIMARY KEY,
    size_w int,
    size_h int,
    hd boolean
);

CREATE TABLE stock (
    id           serial NOT NULL PRIMARY KEY,
    name         varchar(50),
    quantity     int,
    manufacturer varchar(50),
    price        float,
    base_cost    float,
    profit       float generated always as ( price - base_cost ) STORED
);

create table customer (
    id serial not null primary key,
    fname varchar(50),
    lname varchar(50),
    
    /*delivery info*/
    country varchar(50),
    organisation varchar(50),
    region varchar(50), /*state/province/reigion*/
    city varchar(50), /*city/town*/
    street varchar(50),
    house_number varchar(16), /* 3h/28 / 18 / 32U/28 / etc */
    delivery_notes varchar(512),
    
    password_hash varchar(64),
    email varchar(128)
                      
    /* TODO: Investigate payment detail saving */
);

CREATE TABLE orders (
                        id serial not null primary key,
                        customer int,
                        payment_status varchar(20),
                        tracking_number varchar(50),
                        date date,
                        estimated_arrival_time date,
                        total_cost float,
                        order_status varchar(20),

                        CONSTRAINT fk_customer FOREIGN KEY(customer) REFERENCES customer(id)
);

create table pictures_of (
    stock_id int not null,
    photo_id varchar(128) not null primary key,
    
    CONSTRAINT fk_stock FOREIGN KEY(stock_id) REFERENCES stock(id),
    CONSTRAINT fk_photo FOREIGN KEY(photo_id) REFERENCES product_photo(image_uri)
);

create table stock_order (
    stock_id int not null,
    order_id int not null,
    quantity int not null, 
    
    PRIMARY KEY (stock_id, order_id, quantity),
    CONSTRAINT fk_stock FOREIGN KEY (stock_id) REFERENCES stock(id),
    CONSTRAINT fk_order FOREIGN KEY (order_id) REFERENCES orders(id)
);


INSERT INTO hello_table (hello_source, hello_target)
VALUES ('Matt', 'Whit');
