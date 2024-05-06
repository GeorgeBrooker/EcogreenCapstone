import React, { useState } from 'react';
import Popup from '../Popup/Popup';  
import './Usedetail.css'; 

const Usedetail = ({ user, purchases }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedPurchase, setSelectedPurchase] = useState(null);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false); 

  if (!user) return <div>Loading user data...</div>;

  const handleOpenPopup = (purchase) => {
    setSelectedPurchase(purchase);
    setIsOpen(true);
  };

  const handleClosePopup = () => {
    setIsOpen(false);
    setSelectedPurchase(null);
  };

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  return (
    <div className="user-details">
      <h2>User Details</h2>
      <div>Name: {user.name}</div>
      <div>Email: {user.email}</div>
      <div>Address: {user.address}</div>
      <button onClick={toggleDropdown}>Purchase History</button>
      {isDropdownOpen && purchases.length > 0 ? (
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Item Name</th>
              <th>Amount</th>
            </tr>
          </thead>
          <tbody>
            {purchases.map((purchase, index) => (
              <tr key={index} onClick={() => handleOpenPopup(purchase)}>
                <td>{purchase.date}</td>
                <td>{purchase.itemName}</td>
                <td>${purchase.amount}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : isDropdownOpen ? (
        <div>No purchase records found.</div>
      ) : null}
      <Popup isOpen={isOpen} close={handleClosePopup} purchase={selectedPurchase} />
    </div>
  );
};

export default Usedetail;