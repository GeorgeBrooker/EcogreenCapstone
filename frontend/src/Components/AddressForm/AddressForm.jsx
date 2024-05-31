import React, { useState } from "react";
import './AddressForm.css';

const AddressForm = () => {
    const [formData, setFormData] = useState({
        country: "New Zealand",
        street: "",
        suburb: "",
        city: "",
        postcode: "",
        addressId: "",
        building: "",
        company: ""
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value
        });
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        // send data to the server
        console.log("Form submitted:", formData);
    };

    return (
        <div className="address-form">
            <form onSubmit={handleSubmit}>
                <h2>Delivery Address:</h2>
                <div className="form-group">
                    <label htmlFor="country">Country:</label>
                    <select
                        type="text"
                        id="country"
                        name="country"
                        value={formData.country}
                        onChange={handleChange}
                        required
                    >
                        <option value="New Zealand"> New Zealand</option>
                        <option value="Australia"> Australia</option>
                    </select>
                </div>
                <div className="form-group">
                    <label htmlFor="street">House Number & Street:</label>
                    <input
                        type="text"
                        id="street"
                        name="street"
                        value={formData.street}
                        onChange={handleChange}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="suburb">Suburb:</label>
                    <input
                        type="text"
                        id="suburb"
                        name="suburb"
                        value={formData.suburb}
                        onChange={handleChange}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="city">City:</label>
                    <input
                        type="text"
                        id="city"
                        name="city"
                        value={formData.city}
                        onChange={handleChange}
                        required
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="postcode">Postcode:</label>
                    <input
                        type="text"
                        id="postcode"
                        name="postcode"
                        value={formData.postcode}
                        onChange={handleChange}
                        required
                    />
                </div>
                {/* <div className="form-group">
                    <label htmlFor="addressId">Address ID:</label>
                    <input
                        type="text"
                        id="addressId"
                        name="addressId"
                        value={formData.addressId}
                        onChange={handleChange}
                        required
                    />
                </div> */}
                <div className="form-group">
                    <label htmlFor="building">Building (optional):</label>
                    <input
                        type="text"
                        id="building"
                        name="building"
                        value={formData.building}
                        onChange={handleChange}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="company">Company (optional):</label>
                    <input
                        type="text"
                        id="company"
                        name="company"
                        value={formData.company}
                        onChange={handleChange}
                    />
                </div>
                <button type="submit">Submit Address</button>
            </form>
        </div>
    );
};

export default AddressForm;
