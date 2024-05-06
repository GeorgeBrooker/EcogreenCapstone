import React from "react"
import './Popup.css'
const Popup = ({ isOpen, close, purchase }) => {
    if (!isOpen) return null;
  
    return (
      <div className="popup">
        <div className="popup-inner">
          <button onClick={close}>Close</button>
          <h2>Purchase Detail</h2>
          <p>Date: {purchase.date}</p>
          <p>Item: {purchase.itemName}</p>
          <p>Amount: ${purchase.amount}</p>
        </div>
      </div>
    );
  };

  export default Popup

