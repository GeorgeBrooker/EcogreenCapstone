import React, { useState, useContext } from 'react'; 
import './Usedetail.css'; 
import use_logo from '../Assets/login.png'
import { Link } from 'react-router-dom';
import { ShopContext } from '../../Context/ShopContext.jsx';

const Usedetail = ({ user, purchases }) => {
  const { serverUri } = useContext(ShopContext);
  const [isOpen, setIsOpen] = useState(false);
  const [selectedPurchase, setSelectedPurchase] = useState(null);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false); 


  // Password reset logic
  const [isPasswordResetOpen, setIsPasswordResetOpen] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [passForm, setPassForm] = useState({
    verifyCode: "",
    newPassword: "",
    confirmNewPassword: "",
  });
  const passFormHandler = (e) => {
    setPassForm({...passForm, [e.target.name]: e.target.value}); 
  };


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
  
  const handleOpenPasswordReset = async () => {
    const response = await fetch(serverUri + "/api/auth/ResetPassword", {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Email : user.email }),
    });
    
    if (response.ok) {
      alert("Password reset email sent! enter the code below to reset your password.");
      setIsPasswordResetOpen(true);
    }
    else {
      const error = await response.json();
      alert("Failed to send password reset email." + error);
    }
    
  };

  const handleClosePasswordReset = () => {
    setIsPasswordResetOpen(false);
    setPassForm({
        verifyCode: '',
        newPassword: '',
        confirmNewPassword: '',
        });
  };

  const handlePasswordChange = async () => {
    // Validate input
    if (passForm.newPassword !== passForm.confirmNewPassword) {
      setPasswordError('New passwords do not match.');
      return;
    }

  
    
    // Update password
    const updateResponse = await fetch(serverUri + "/api/auth/ConfirmResetPassword", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ Email: user.email, Pass : passForm.newPassword, Code: passForm.verifyCode }),
    });
    if (!updateResponse.ok) {
        setPasswordError(await updateResponse.text());
        return;
    }
    alert("Password updated successfully!");
    handleClosePasswordReset();
  };

  const convertTime = (time) => {
    const date = new Date(time);
    const options = { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', second: '2-digit' };
    const formattedDate = new Intl.DateTimeFormat('default', options).format(date);
    const [month, day, year, ...rest] = formattedDate.split(' ');
    return `${day} ${month} ${rest.join(' ')} ${year.slice(0, -1)}`;
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
              <th>Date</th>
              <th>Order ID</th>
              <th>Order Status</th>
              <th>Tracking Number</th>
              <th>Cost (NZD)</th>
            </tr>
          </thead>
          <tbody>
            {purchases.map((purchase, index) => (
              <tr key={index}>
                <td>{convertTime(purchase.date)}</td>
                <td>{purchase.id}</td>
                <td>{purchase.status}</td>
                <td>{purchase.trackingNumber}</td>
                <td>${purchase.totalCost}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : isDropdownOpen ? (
        <div>No purchase records found.</div>
      ) : null}
      <br />
      <button className="change-password" onClick={handleOpenPasswordReset}>Change Password</button>
        {isPasswordResetOpen && (
          <div className="password-reset">
            
            <div>
              <label>Verification Code:</label>
              <input name = "verifyCode"
                type="text"
                value={passForm.verifyCode}
                onChange={passFormHandler}
              />
            </div>
            <div>
              <label>New Password:</label>
              <input name= "newPassword"
                type="password"
                value={passForm.newPassword}
                onChange={passFormHandler}
              />
            </div>
            <div>
              <label>Confirm New Password: </label>
              <input name = "confirmNewPassword"
                type="password"
                value={passForm.confirmNewPassword}
                onChange={passFormHandler}
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
 