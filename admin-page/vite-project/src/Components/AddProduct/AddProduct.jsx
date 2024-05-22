import React, { useState } from "react";
import { Carousel } from 'react-responsive-carousel';
import "react-responsive-carousel/lib/styles/carousel.min.css";
import upload_area from "../../assets/file.png"
import {serverUri} from "../../App.jsx";
import './AddProduct.css'
const AddProduct = () =>{
    const[uploadCandidates, setUploadCandidates] = useState([]); // State of image uploader
    const[showCarousel, setShowCarousel] = useState(false); // State of carousel
    const[activeSlide, setActiveSlide] = useState(0); //Get and set current slide of the carousel
    const [productDetails,setproductDetails] = useState({// it will change, when the api finished.
        Name:'',
        image:'',
        discount:'',
        price:'',
        quantity:'',
    })
    const deleteImageHandler = (index) => {
        setUploadCandidates(prevState => prevState.filter((_, i) => i !== index));
        setActiveSlide(index === 0 ? 0 : index - 1);
    };
    const imageUploadHandler = (e) =>{
        if (e.target.files.length > 8 || uploadCandidates.length + e.target.files.length > 8) {
            alert('There can be a maximum of 8 images per stock item');
            return;
        }
        console.log(e.target.files);
        setUploadCandidates(prevState => [...prevState, ...e.target.files]);
        setShowCarousel(true); // Show the carousel if there are images to display
    };
    const changeHandler=(e)=>{
        setproductDetails({...productDetails,[e.target.name]:e.target.value})
    }
    const Add_Product = async() =>{
        // TODO add form validation
        console.log(productDetails);
        let stockInput = {
            "Name": productDetails.name,
            "Description": productDetails.description,
            "Price": productDetails.price,
            "DiscountPercentage": productDetails.discount,
            "TotalStock": productDetails.quantity,
            "CreateWithoutStripeLink": false
        };
        // First add stock to local DB, only after succesfull local addition should we try and persist to S3
        // (this is to avoid orphaned images in S3 which are more annoying to clean up than local DB entries)
        const response = await fetch(serverUri + '/api/shop/AddStock', {
            method: 'POST',
            headers:{
                'Content-Type':'application/json',
            },
            body: JSON.stringify(stockInput),
        });
        if (!response.ok) {
            console.log(`Failed to add stock with response: ${response.statusText}`);
            return;
        }

        // Product added successfully to local DB, now we must upload images to S3
        console.log("Product added successfully, beginning image upload...");
        const stockId = await response.json();
        
        let formData = new FormData();
        const uploadPromises = uploadCandidates.map((file, index) => {
            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => {
                    formData.append(`file${index}`, reader.result.split(',')[1]); // Remove data URL prefix
                    resolve();
                };
                reader.onerror = reject;
                reader.readAsDataURL(file);
            });
        })
        
        await Promise.all(uploadPromises); // Wait for all images to be read and appended to form data
        
        const imageResponse = await fetch(serverUri + `/api/inventory/UploadPhotos/${stockId}`, {
            method: 'POST',
            body: formData
        });
        
        if (imageResponse.ok) {
            // images uploaded successfully
            console.log("Images uploaded successfully");
        } else {
            console.log(`Image upload failed with response: ${imageResponse.statusText}`);
            
            // Cleanup failed persist
            const deleteResponse = await fetch(serverUri + `/api/shop/DeleteStock/${stockId}`, {
                method: 'DELETE'
            });
            if (deleteResponse.ok) {
                console.log("Failed stock addition cleaned up successfully");
            } else {
                console.log("Failed to clean up after stock addition failure with response: " + deleteResponse.statusText);
            }
        }
    }
        
        
    
  return(
    <div className="add-product">
        <h1 className="modify-menu-header">Add Stock</h1>
        <div className="addproduct-itemfield">
            <p>Product Name</p>
            <input  value={productDetails.name} onChange={changeHandler} type="text" name="name" placeholder="Type here"></input>
        </div>
        <div className="addproduct-itemfield">
            <p>Product Description</p>
            <input value={productDetails.description} onChange={changeHandler} type="text" name="description" placeholder="Type here"></input>
        </div>
        <div className="addproduct-price">
            <div className="addproduct-itemfield">
                <p>Base Price</p>
                <input value={productDetails.price} onChange={changeHandler} type="text" name='price'
                       placeholder="Type here"></input>
            </div>

            <div className="addproduct-itemfield">
                <p>Discount Percentage</p>
                <input value={productDetails.discount} onChange={changeHandler} type="number"
                       name='discount' placeholder="Type here"></input>
            </div>
            <div className="addproduct-itemfield">
                <p>Total Quantity</p>
                <input value={productDetails.quantity} onChange={changeHandler} type="number"
                       name='quantity' placeholder="Type here"></input>
            </div>
        </div>
        <div className="addproduct-imagefield">
            <label htmlFor="file-input" className="addproduct-imageuploader">
                <p>Upload Stock Images (8 maximum)</p>
            </label>
            <input onChange={imageUploadHandler} type="file" multiple name="image" id="file-input"/>
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
        
        <button onClick={()=>{Add_Product()}}className="addproduct-btn">Add</button>

         



    </div>
  )
}
export default AddProduct