import React from "react"
import './Hero.css'
import coconut from '../Assets/coconut.png'
import charcoal from '../Assets/charcoal.png'
import hand_icon from '../Assets/hand_icon.png'
import arrow_icon from '../Assets/arrow.png' 
import { useState } from 'react'


 
const Hero = () => {

    const [img, setImage] = useState(coconut); 
    const [animate, setAnimate] = useState(false);

    const toggleImage = () => {
        setAnimate(true);
        setTimeout(()=> {
            setImage(currentImage => currentImage === coconut ? charcoal : coconut);
            setAnimate(false);
        }, 1200);
    };

    return (
        <div className="hero">
            <div className="hero-left">
                <h2>Ecogreen</h2>
                <div>
                    
                    <p>The way towards a sustainable future.</p>
                </div>
                <div className="hero-latest-btn">
                    <div>Our Product</div>
                    <img src={arrow_icon} alt="" />
                </div>
            </div>
            <div className="hero-right">
                <div className="coconut-burn">
                    <img src={img} alt=""  onClick={toggleImage} className={animate ? 'burn' : ''}/>
                </div>
                <div className="hero-hand-icon">
                    <img src={hand_icon} alt="" />
                    <p>Burn this coconut!</p>
                </div>
                
            </div>
        </div>
    )
}

export default Hero