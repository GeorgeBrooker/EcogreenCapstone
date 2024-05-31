import React, { useState } from 'react';
import './CSS/ContactUs.css';
// import ContactFooter from'../Components/ContactFooter/ContactFooter'
 import location from '../Components/Assets/location.png'
 import whatsapp from '../Components/Assets/whatsapp.png'
 import email from '../Components/Assets/email.png'

function ContactUs() {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    message: '',
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prevState => ({
      ...prevState,
      [name]: value,
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    // send the data to a server
    alert('Thank you for contacting us!');
  };

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
        <button type="submit">Send</button>
      </form>
    </div>
    <div className='details'>
      <div className="mobile">
        <img src={whatsapp} alt="" />
        <p>Mobile/WhatsApp: +64 22 648 2838</p>
      </div>
      <div className="address">
        <img src={location} alt="" />
        <a href='https://www.google.com/maps/place/21A+Margate+Road,+Blockhouse+Bay,+Auckland+0600/@-36.9120189,174.704206,16z/data=!3m1!4b1!4m6!3m5!1s0x6d0d46a4b63f5d83:0xd9a7901a2c7fd65b!8m2!3d-36.9120189!4d174.704206!16s%2Fg%2F11c21xcyl6?entry=ttu' target='_blank' rel='noreferrer'>21A, Margate Road, Blockhouse Bay, Auckland 0600, New Zealand</a>
      </div>
      
      <div className="email">
        <img src={email} alt="" />
        <p>uttam@kashish.co.nz</p>
      </div>
    </div>
    </div>
    
  );
}

export default ContactUs;
