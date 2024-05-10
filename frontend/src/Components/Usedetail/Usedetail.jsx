import React, { useState } from 'react';
import Popup from '../Popup/Popup';  
import './Usedetail.css'; 
import use_logo from '../Assets/login.png'
import { Link } from 'react-router-dom';

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
    <div className='parent-container'> 
     
    <div className="user-details">
    <div>
    <img src={use_logo} alt="user" className='user-image' />
  </div>  
      <h2>User Details</h2>
      <div>Name: {user.name}</div>
      <div>Email: {user.email}</div>
       
      <button onClick={toggleDropdown}>Purchase History</button>
      {isDropdownOpen && purchases.length > 0 ? (
        <table>
          <thead>
            <tr>
              <th>trackingNumber</th>
              <th>packageReference</th>
              <th>id</th>
            </tr>
          </thead>
          <tbody>
            {purchases.map((purchase, index) => (
              <tr key={index} onClick={() => handleOpenPopup(purchase)}>
                <td>{purchase.trackingNumber}</td>
                <td>{purchase.packageReference}</td>
                <td>${purchase.id}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : isDropdownOpen ? (
        <div>No purchase records found.</div>
      ) : null}
      <Popup isOpen={isOpen} close={handleClosePopup} purchase={selectedPurchase} />
      <div className='contact-link'> 
      <Link to="/contactUs">npm install nodemailerIf you have any questions, please contact us</Link>
     
      </div>
    </div>
    </div>
  );
};

export default Usedetail;
 