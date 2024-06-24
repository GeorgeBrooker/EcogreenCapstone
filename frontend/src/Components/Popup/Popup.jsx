import React from "react"
import './Popup.css'
import { serverUri } from '../../Context/ShopContext.jsx';
const Popup = async ({ isOpen, close, purchase }) => {
    if (!isOpen) return null;

    console.log("lineitems" + lineItems);
    return (
        <div className="popup">
            {lineItems.map((item, index) => (
                <div key={index} className="popup-inner">
                    <h2>Purchase Detail</h2>
                    <p>Date: {purchase.date}</p>
                    <p>Item: {purchase.itemName}</p>
                    <p>Amount: ${purchase.amount}</p>
                </div>
            ))}
            <button onClick={close}>Close</button>
        </div>
    );
};

export default Popup

