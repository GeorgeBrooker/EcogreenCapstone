import React from "react"
import './Hero.css'
import coco from '../Assets/coco.png'
import hand_icon from '../Assets/hand_icon.png'
import arrow_icon from '../Assets/arrow.png' 
 
const Hero = () => {
    return (
        <div className="hero">
            <div className="hero-left">
                <h2>New Arrival</h2>
                <div>
                    <div className="hero-hand-icon">
                        <p>new</p>
                        <img src={hand_icon} alt="" />
                    </div>
                    <p>products</p>
                    <p>for you</p>
                </div>
                <div className="hero-latest-btn">
                    <div>Latest Products</div>
                    <img src={arrow_icon} alt="" />
                </div>
            </div>
            <div className="hero-right">
                <img src={coco} alt="" />
            </div>
        </div>
    )
}

export default Hero