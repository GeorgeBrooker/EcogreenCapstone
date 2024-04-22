import React from 'react';
import './QuantityStepper.css'
import remove_icon from '../Assets/remove_icon.png';
import add_icon from '../Assets/add_icon.png';


const QuantityStepper = ({ quantity, onIncrease, onDecrease }) => {
  return (
    <div className="quantity-stepper">
      <button onClick={onDecrease}><img src={remove_icon} alt="" /></button>
      <span>{quantity}</span>
      <button onClick={onIncrease}><img src={add_icon} alt="" /></button>
    </div>
  );
};

export default QuantityStepper;
