import React, { useEffect, useState } from "react";
import './ListCustomers.css';

const serverUri = "http://localhost:3000";

const ListCustomers = () => {
    const [allCustomers, setCustomers] = useState([]);

    const fetchInfo = async () => {
        const response = await fetch(`${serverUri}/api/shop/GetCustomers`);
        const data = await response.json();
        setCustomers(data);
    };

    useEffect(() => {
        fetchInfo();
    }, []);

    return (
        <div className="listcustomers">
            <h1>All Customers List</h1>
            <div className="listcustomers-format-main listcustomers-header">
                <div className="column id">ID</div>
                <div className="column stripeId">Stripe ID</div>
                <div className="column name">Name</div>
                <div className="column email">Email</div>
            </div>
            <div className="listcustomers-allcustomers">
                {allCustomers.map((customer, index) => (
                    <div key={index} className="listcustomers-item">
                        <div className="column id">{customer.id}</div>
                        <div className="column stripeId">{customer.stripeId}</div>
                        <div className="column name">{customer.firstName} {customer.lastName}</div>
                        <div className="column email">{customer.email}</div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default ListCustomers;
