import React, { useState } from 'react';
import Popup from '../Popup/Popup';  
import './Usedetail.css'; 
import use_logo from '../Assets/login.png'
import { Link } from 'react-router-dom';

const Usedetail = ({ user, purchases }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedPurchase, setSelectedPurchase] = useState(null);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false); 


  const [isPasswordResetOpen, setIsPasswordResetOpen] = useState(false);
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState('');



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




  const handleOpenPasswordReset = () => {
    setIsPasswordResetOpen(true);
  };

  const handleClosePasswordReset = () => {
    setIsPasswordResetOpen(false);
    setOldPassword('');
    setNewPassword('');
    setConfirmNewPassword('');
    setPasswordError('');
  };

  const handlePasswordChange = async () => {
    if (newPassword !== confirmNewPassword) {
      setPasswordError('New passwords do not match.');
      return;
    }

    // Here we should verify the old password with backend server.
    if (oldPassword !== "old password") {
      setPasswordError('Old password is incorrect.');
      return;
    }
    try {
      // Verify old password with backend
      const verifyResponse = await fetch('/verifyPassword', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email: user.email, oldPassword }),
      });
      
      const verifyData = await verifyResponse.json();

      if (!verifyData.success) {
        setPasswordError('Old password is incorrect.');
        return;
      }

      // If old password is correct, update to new password
      const updateResponse = await fetch('/updatePassword', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email: user.email, newPassword }),
      });

      const updateData = await updateResponse.json();

      if (updateData.success) {
        setPasswordSuccess('Password changed successfully!');
        handleClosePasswordReset();
      } else {
        setPasswordError('Failed to change password. Please try again.');
      }
    } catch (error) {
      setPasswordError('An error occurred. Please try again.');
    }
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
      <div className="purchase-history-button">
      <button onClick={toggleDropdown}>Purchase History</button>
      </div>
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

      <br />
      <button className="change-password" onClick={handleOpenPasswordReset}>Change Password</button>
        {isPasswordResetOpen && (
          <div className="password-reset">
            
            <div>
              <label>Old Password: </label>
              <input
                type="password"
                value={oldPassword}
                onChange={(e) => setOldPassword(e.target.value)}
              />
            </div>
            <div>
              <label>New Password: </label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
              />
            </div>
            <div>
              <label>Confirm New Password: </label>
              <input
                type="password"
                value={confirmNewPassword}
                onChange={(e) => setConfirmNewPassword(e.target.value)}
              />
            </div>
            {passwordError && <div className="error">{passwordError}</div>}
            <button onClick={handlePasswordChange}>Submit</button>
            <button onClick={handleClosePasswordReset}>Cancel</button>
          </div>
        )}


      <div className='contact-link'> 
        <Link to="/contactUs">If you have any questions, please contact us</Link>
      </div>
    </div>
    </div>
  );
};

export default Usedetail;
 