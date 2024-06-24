import React, { useEffect, useState } from "react";
import './ListOrder.css';
import {serverUri, apiEndpoint, getSessionTokens, theme, fetchWithAuth } from "../../App.jsx";
import Modal from "../Modal/Modal";
import Nzpostdetail from '../Nzpostdetail/Nzpostdetail'
import {Button, CircularProgress, FormControlLabel, Switch, ThemeProvider} from "@mui/material";

const ListOrder = () => {
    const [orders, setOrders] = useState([]);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [orderUpdating, setOrderUpdating] = useState(false);

    const fetchInfo = async () => {
        // Check if order data is cached locally and use that if available, always refetch regardless to ensure up to date data.
        let orderInfo = sessionStorage.getItem("orders");
        if (orderInfo !== null) {
            let data = JSON.parse(orderInfo);
            setOrders(data);
        }
        else {
            setOrderUpdating(true);
        }
        
        
        const response = await fetchWithAuth(`${serverUri}${apiEndpoint}/GetOrders`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });
        let data = await response.json();
        data = data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        sessionStorage.setItem("orders", JSON.stringify(data));// Cache order data locally, refetch on modification
        setOrders(data);
        
        console.log("Updated order data")
        setOrderUpdating(false);
    };
    
    const convertTime = (time) => {
        const date = new Date(time);
        const options = { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
        const formattedDate = new Intl.DateTimeFormat('default', options).format(date);
        const [month, day, year, ...rest] = formattedDate.split(' ');
        return `${day} ${month} ${rest.join(' ')} ${year.slice(0, -1)}`;
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
        <ThemeProvider theme={theme}>

            <div className={"page-headers"}>
                <h1 className="title">Orders</h1>
            </div>
            <div className="list-order">
                <div className="listorder-format listorder-header">
                    <p className={"col-1"}>#</p>
                    <p className={"col-2"}>Customer</p>
                    <p className={"col-3"}>Ammount</p>
                    <p className={"col-4"}>Status</p>
                    <p className={"col-5"}>Delivery Label</p>
                    <p className={"col-6"}>Time</p>
                </div>

                {orderUpdating && <div style={{
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    height: '100vh'
                }}>
                    <CircularProgress/>
                    <span>Loading...</span>
                </div>}

                {!orderUpdating ? (
                    <div className="listorder-allorder">
                        {orders.map((order, index) => (
                            <div key={index} className="listorder-format listorder-item"
                                 onClick={(e) => handleOrderClick(e, order.id)}>
                                <p className={"col-1"}>{index + 1}</p>
                                <p className={"col-2"}>{order.customerName}</p>
                                <p className={"col-3"}>${order.orderCost}</p>
                                <p className={"col-4"}>{order.orderStatus}</p>
                                <p className={"col-5"}>{order.deliveryLabelUid}</p>
                                <p className={"col-6"}>{convertTime(order.createdAt)}</p>
                            </div>
                        ))}
                        {(orders.length === 0 && !orderUpdating) && <p className="no-order-message">No orders available</p>}
                    </div>
                ) : null}
            </div>
        </ThemeProvider>
    );
}

export default ListOrder;
