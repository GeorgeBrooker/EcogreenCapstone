import React from "react"
import './Footer.css'
import footer_logo from '../Assets/logo.png'
import instagram_icon from '../Assets/insta.png'
import whatsapp_icon from '../Assets/whatsapp.png'
import location_icon from '../Assets/location.png'
import email_icon from '../Assets/email.png'

const Footer = () => {
    return (
        <div className="footer">
            <div className={"footer-content"}>
                <div className="footer-logo">
                    <img src={footer_logo} alt=""/>
                    <p>Ecogreen</p>
                    <div className="bottom-bar-content">
                        <p>Kashish Enterprises Limited</p>
                    </div>
                </div>
                <div className="bottom-bar">
                    <div className="Contactbottom-bar">
                        <div className="Contactbottom-bar-content">
                            <div className={"footer-item-container"}>
                                <img src={location_icon}/>
                                <p>Address:<br/>21A, Margate Road, Blockhouse Bay, Auckland 0600, New Zealand</p>
                            </div>
                            <div className={"footer-item-container"}>
                                <img src={whatsapp_icon}/>
                                <p>Mobile/WhatsApp:<br/>+64 22 648 2838</p>
                            </div>
                            <div className={"footer-item-container"}>
                                <img src={email_icon}/>
                                <p>Email:<br/>uttam@kashish.co.nz</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div className="footer-copyright">
                <p>Copyright @2024 - All Right Reserved</p>
            </div>
        </div>
    )
}

export default Footer