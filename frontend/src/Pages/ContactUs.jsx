import React, { useState, useContext } from 'react';
import './CSS/ContactUs.css';
import { ShopContext } from '../Context/ShopContext';
import { Button, CircularProgress } from "@mui/material";
import { styled } from '@mui/material/styles';

function ContactUs() {
    const {serverUri} = useContext(ShopContext);
    const [formData, setFormData] = useState({
      name: '',
      email: '',
      message: '',
    });

    const [isLoading, setIsLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prevState => ({
      ...prevState,
      [name]: value,
    }));
  };

  const handleSubmit = async (e)  => {
    e.preventDefault();

    setIsLoading(true); // Start loading
    
    const response = await fetch(serverUri + '/api/shop/ContactUs', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
    });

    setIsLoading(false); // Stop loading

    if (!response.ok) {
      alert('An error occurred while submitting the form' + response.statusText);
    }
    else{
      alert('Thank you for contacting us!');
    }
    setFormData({
      name: '',
      email: '',
      message: '',
    });
    
  };

  const SendButton = styled(Button)({
    borderRadius: '75px',
    fontSize: '15px',
    fontWeight: 'bold',
    padding: '10px 20px',
    backgroundColor: '#24831a',
    color: '#fff',
    '&:hover': {
      backgroundColor: '#475247',
    },
    '&:disabled': {
      backgroundColor: '#cccccc',
    },
  });

  return (
    <div className="contactUs">
    <div className="contact-us-container">
      <h2>Contact Us</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
          />
        </div>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
          />
        </div>
        <div className="form-group">
          <label htmlFor="message">Message</label>
          <textarea
            id="message"
            name="message"
            value={formData.message}
            onChange={handleChange}
          />
        </div>
        <SendButton type="submit" onClick={handleSubmit} disabled={isLoading}>
          {isLoading ? <CircularProgress size={24} /> : 'Send'}
        </SendButton>
      </form>
    </div>
    </div>
    
  );
}

export default ContactUs;
