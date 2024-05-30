import React from "react"
import './ContactFooter.css'

import footer_logo from '../Assets/logo.png'
 

const ContactFooter = () => {
    return (
        <div className=" Contactfooter">
            <div className=" Contactfooter-logo">
                 
              
            </div>
            <div className="Contactbottom-bar">
                <div className="Contactbottom-bar-content">
                    <p>Address: 21A, Margate Road, Blockhouse Bay, Auckland 0600, New Zealand</p>
                    <p>Mobile/WhatsApp: +64 22 648 2838</p>
                    <p>Email: uttam@kashish.co.nz</p>
                </div>
            </div>
            <div className="Contactfooter-social-icon">
                 
            </div>
            <div className="Contactfooter-copyright">
                <hr />
                <p>Copyright @2024 - All Right Reserved</p>
            </div>
        </div>
    )
}

export default ContactFooter