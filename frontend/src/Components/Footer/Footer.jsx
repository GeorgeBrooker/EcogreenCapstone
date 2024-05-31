import React from "react"
import './Footer.css'
import footer_logo from '../Assets/logo.png'
import instagram_icon from '../Assets/insta.png'

const Footer = () => {
    return (
        <div className="footer">
            <div className="footer-logo">
                <img src={footer_logo} alt="" />
                <p>Ecogreen</p>
            </div>
            <div className="bottom-bar">
                <div className="bottom-bar-content">
                    <p>Kashish Enterprises Limited</p>
                </div>
            </div>
            <div className="footer-social-icon">
                <div className="footer-icons-container">
                    <img src={instagram_icon} alt="" />
                </div>
            </div>
            <div className="footer-copyright">
                <hr />
                <p>Copyright @2024 - All Right Reserved</p>
            </div>
        </div>
    )
}

export default Footer