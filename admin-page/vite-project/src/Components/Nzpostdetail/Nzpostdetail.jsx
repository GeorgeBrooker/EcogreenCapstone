import './Nzpostdetail.css'
import React, { useState } from "react";
import PropTypes from 'prop-types';

const ParcelInformation = ({ onSave }) => {
    const [parcelDetails, setParcelDetails] = useState({
        height: '',
        length: '',
        width: '',
        weight: ''
    });

    const changeHandler = (e) => {
        setParcelDetails({ ...parcelDetails, [e.target.name]: e.target.value });
    };

    const saveParcel = () => {
        // Add validation or additional logic before saving
        onSave(parcelDetails);
    };

    return (
        <div className="parcel-info-container">
            <h1 className="parcel-info-header">Parcel Information</h1>
            <div className="parcel-info-field">
                <p>Height (cm)</p>
                <input value={parcelDetails.height} onChange={changeHandler} type="number" name="height" placeholder="Enter height" />
            </div>
            <div className="parcel-info-field">
                <p>Length (cm)</p>
                <input value={parcelDetails.length} onChange={changeHandler} type="number" name="length" placeholder="Enter length" />
            </div>
            <div className="parcel-info-field">
                <p>Width (cm)</p>
                <input value={parcelDetails.width} onChange={changeHandler} type="number" name="width" placeholder="Enter width" />
            </div>
            <div className="parcel-info-field">
                <p>Weight (kg)</p>
                <input value={parcelDetails.weight} onChange={changeHandler} type="number" name="weight" placeholder="Enter weight" />
            </div>
            <button onClick={saveParcel} className="parcel-info-btn">Save Parcel Information</button>
        </div>
    );
};

ParcelInformation.propTypes = {
    onSave: PropTypes.func.isRequired
};

export default ParcelInformation;
