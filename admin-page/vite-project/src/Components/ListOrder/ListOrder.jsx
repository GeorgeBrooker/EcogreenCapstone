import React, { useEffect, useState } from "react";
import './ListOrder.css';
import {serverUri} from "../../App.jsx";

const ListOrder = () => {
    const [allCustomers, setCustomers] = useState([]);

    const fetchInfo = async () => {
        const response = await fetch(`${serverUri}/api/shop/GetOrders`);
        const data = await response.json();
        setCustomers(data);
    };

    useEffect(() => {
        fetchInfo();
    }, []);

    return (
        <div className="listorder">
            <h1>All Order</h1>
            <div className="listorder-format-main listorder-header">
                <div className="column id">id</div>
                <div className="column customerId">customerId</div>
                <div className="column trackingNumber">trackingNumber</div>
                <div className="column paymentIntentId">paymentIntentId</div>
                <div className="column deliveryLabelUid">deliveryLabelUid</div>
                <div className="column time">time</div>
                <div className="column amount">amount</div>
                 
            </div>
            <div className="listorder-allorder">
                {allCustomers.map((Order, index) => (
                    <div key={index} className="listorder-item">
                        <div className="column id">{Order.id}</div>
                        <div className="column customerId">{Order.customerId}</div>
                        <div className="column trackingNumber">{Order.trackingNumber}</div>
                        <div className="column paymentIntentId">{Order.paymentIntentId}</div>
                        <div className="column deliveryLabelUid">{Order.deliveryLabelUid}</div>
                        <div className="column time"></div>
                        <div className="column amount"></div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default ListOrder;
