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

    const [addressSuggestions, setAddressSuggestions] = useState([]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({
            ...formData,
            [name]: value
        });

        if (name === "street") {
            fetchAddressSuggestions(value);
        }
    };

    const fetchAddressSuggestions = async (query) => {
        if (query.length > 2) { // 当输入长度大于2时触发搜索
            try {
                const response = await fetch(`/api/address/search-addresses?query=${query}`);
                if (response.ok) {
                    const data = await response.json();
                    console.log("Fetched data:", data); // 添加日志记录返回的内容
                    setAddressSuggestions(data.addresses); // 假设 data.addresses 是地址建议的数组
                } else {
                    console.error("Error fetching address suggestions:", response.statusText);
                    const errorText = await response.text();
                    console.error("Error response text:", errorText);
                }
            } catch (error) {
                console.error("Error fetching address suggestions:", error);
            }
        }
    };
    
    
    
    

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log("Form submitted:", formData);
        // Further submission logic...
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
                {addressSuggestions.map((suggestion, index) => (
                    <div key={index}>
                        {suggestion.full_address} {/* 假设 suggestion 对象有一个 full_address 属性 */}
                    </div>
                ))}
                <button type="submit">Submit Address</button>
            </form>
        </div>
    );
};

export default AddressForm;

