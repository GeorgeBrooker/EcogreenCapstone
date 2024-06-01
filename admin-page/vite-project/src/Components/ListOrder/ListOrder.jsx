import React, { useEffect, useState } from "react";
import './ListOrder.css';
import {serverUri} from "../../App.jsx";
import Modal from "../Modal/Modal";
import Nzpostdetail from '../Nzpostdetail/Nzpostdetail'

const ListOrder = () => {
    const [allCustomers, setCustomers] = useState([]);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);


    const fetchInfo = async () => {
        const response = await fetch(`${serverUri}/api/shop/GetOrders`);
        const data = await response.json();
        setCustomers(data);
    };

    useEffect(() => {
        fetchInfo();
    }, []);
    const openModal = (order) => {
        setSelectedOrder(order);
        setIsModalOpen(true);
    };

    const closeModal = () => {
        setIsModalOpen(false);
    };

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
                <div className="parcelinformation">parcel</div>
                 
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
                        <button className = 'detail' onClick={() => openModal(Order)}>Detail</button>
                    </div>
                ))}
            </div>
            {isModalOpen && (
                <Modal isOpen={isModalOpen} onClose={closeModal}>
                    <Nzpostdetail order={selectedOrder} />
                </Modal>
                )}
        </div>
    );
}

export default ListOrder;
