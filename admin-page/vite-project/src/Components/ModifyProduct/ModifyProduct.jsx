import React, { useState, useEffect } from "react";
import PropTypes from 'prop-types';
import { Carousel } from 'react-responsive-carousel';
import "react-responsive-carousel/lib/styles/carousel.min.css";
import { serverUri, fetchWithAuth } from "../../App.jsx";
import {Button} from "@mui/material";
import {CircularProgress} from "@mui/material";
 

const ModifyProduct = ({ productId, onClose, onSave }) => {
    const [isLoading, setIsLoading] = useState(false);
    const all_products = JSON.parse(localStorage.getItem("allProducts"));
    const [uploadCandidates, setUploadCandidates] = useState([]);
    const [showCarousel, setShowCarousel] = useState(false);
    const [activeSlide, setActiveSlide] = useState(0);
    const [productDetails, setProductDetails] = useState({
        name: '',
        description: '',
        price: '',
        discount: '',
        quantity: '',
    });

    useEffect(() => {
        const fetchProductDetails = async () => {
            const data = all_products.find((product) => product.id === productId);
            setProductDetails({
                name: data.name,
                description: data.description,
                price: data.price, 
                discount: data.discountPercentage,
                quantity: data.totalStock,
            });
        };
        if (productId) {
            fetchProductDetails();
        }
    }, [productId]);

    const deleteImageHandler = (index) => {
        setUploadCandidates(prevState => prevState.filter((_, i) => i !== index));
        setActiveSlide(index === 0 ? 0 : index - 1);
    };

    const imageUploadHandler = (e) => {
        if (e.target.files.length > 8 || uploadCandidates.length + e.target.files.length > 8) {
            alert('There can be a maximum of 8 images per stock item');
            return;
        }
        setUploadCandidates(prevState => [...prevState, ...e.target.files]);
        setShowCarousel(true);
    };

    const changeHandler = (e) => {
        setProductDetails({ ...productDetails, [e.target.name]: e.target.value });
    };

    const saveChanges = async () => {
        setIsLoading(true);
        await onSave(productId, productDetails);
        setIsLoading(false);
        onClose();
    };

    return (
        <div className="add-product">
            <h1 className="modify-menu-header">Modify Product</h1>
            <div className="addproduct-itemfield">
                <p>Product Name</p>
                <input value={productDetails.name} onChange={changeHandler} type="text" name="name" placeholder="Type here" />
            </div>
            <div className="addproduct-itemfield">
                <p>Product Description</p>
                <input value={productDetails.description} onChange={changeHandler} type="text" name="description" placeholder="Type here" />
            </div>
            <div className="addproduct-price">
                <div className="addproduct-itemfield">
                    <p>Base Price</p>
                    <input value={productDetails.price} onChange={changeHandler} type="text" name='price' placeholder="Type here" />
                </div>
                <div className="addproduct-itemfield">
                    <p>Discount Percentage</p>
                    <input value={productDetails.discount} onChange={changeHandler} type="number" name='discount' placeholder="Type here" />
                </div>
                <div className="addproduct-itemfield">
                    <p>Total Quantity</p>
                    <input value={productDetails.quantity} onChange={changeHandler} type="number" name='quantity' placeholder="Type here" />
                </div>
            </div>
            <div className="addproduct-imagefield">
                <label htmlFor="file-input" className="addproduct-imageuploader">
                    <p>Upload Stock Images (8 maximum)</p>
                </label>
                <input onChange={imageUploadHandler} type="file" multiple name="image" id="file-input" />
                {showCarousel && uploadCandidates.length > 0 && (
                    <Carousel className="custom-carousel" selectedItem={activeSlide} onChange={setActiveSlide}>
                        {Array.from(uploadCandidates).map((image, index) => (
                            <div key={index} className="carousel-image">
                                <button onClick={() => deleteImageHandler(index)} className="imageupload-delete">X</button>
                                <img src={URL.createObjectURL(image)} alt="upload" className="imageupload-thumbnail" />
                            </div>
                        ))}
                    </Carousel>
                )}
            </div>
            <Button variant="contained" disabled={isLoading} onClick={saveChanges} className="addproduct-btn" style={{fontSize: "15px", fontWeight: "bold"}}>
                {isLoading ? <CircularProgress size={24} /> : 'Save Changes'}
            </Button>
            
        </div>
    );
};

ModifyProduct.propTypes = {
    productId: PropTypes.any.isRequired,
    onClose: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
};

export default ModifyProduct;
