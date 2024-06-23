import React, { useContext, useState } from "react";
import './ProductDisplay.css'
import { ShopContext } from "../../Context/ShopContext";
import star_icon from '../Assets/star_icon.png'
import star_dull_icon from '../Assets/star_dull_icon.png'
import { Link } from 'react-router-dom';
import { Button, CircularProgress } from "@mui/material";
import { styled } from '@mui/material/styles';


const ProductDisplay = (props) =>{
    const {product} = props;
    const {addToCart} = useContext(ShopContext);
    const [mainImage, setMainImage] = useState(product.image + "1.jpeg");
    const [isLoading, setIsLoading] = useState(false);

    const handleImageClick = (image) => {
        setMainImage(image);
    };

    const getImageClass = (image) => {
        return image === mainImage ? 'productdisplay-img-item selected' : 'productdisplay-img-item';
    };

    const AddToCartButton = styled(Button)({

        borderRadius: '50px', 
        fontSize: '15px',
        fontWeight: 'bold',
        
        backgroundColor: '#1976d2',
        color: '#fff',
        '&:hover': {
            backgroundColor: '#115293',
        },
        '&:disabled': {
            backgroundColor: '#cccccc',
        },
    });

    const BackToShopButton = styled(Button)({
        borderRadius: '50px',
        backgroundColor: '#ff4141', 
        color: '#FFFFFF', 
        borderRadius: '50px', 
        fontWeight: '600', 
        fontSize: '10px', 
        '&:hover': {
            backgroundColor: '#115293', 
        },
    });

    return(
        <div className="productdisplay">
            <div className="productdisplay-left">
                <div className="productdisplay-img-list">
                    <img className={getImageClass(product.image + "1.jpeg")} src={product.image + "1.jpeg"} alt=''onClick={() => handleImageClick(product.image + "1.jpeg")}/>
                    <img className={getImageClass(product.image + "2.jpeg")} src={product.image + "2.jpeg"} alt=''onClick={() => handleImageClick(product.image + "2.jpeg")}/>
                    <img className={getImageClass(product.image + "3.jpeg")} src={product.image + "3.jpeg"} alt=''onClick={() => handleImageClick(product.image + "3.jpeg")}/>
                    <img className={getImageClass(product.image + "4.jpeg")} src={product.image + "4.jpeg"} alt=''onClick={() => handleImageClick(product.image + "4.jpeg")}/>
                </div>
                <div className="productdisplay-img">
                    <img className='productdisplay-main-img' src={mainImage} alt='' />
                </div>

            </div>
            <div className="productdisplay-right">
                <h1>{product.name}</h1>
                
                <div className="productdisplay-right-stars">
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_icon} alt="" />
                    <img src={star_dull_icon} alt="" />
                    <p>(111)</p>
                </div>
                <div className="productdisplay-right-prices">
                    <div className="productdisplay-right-price-old">
                        ${product.old_price}
                    </div>
                    <div className="productdisplay-right-price-new">
                        ${product.new_price}
                    </div>
                </div>
                <div className="productdisplay-right-description">
                    {product.description}
                </div>
                <div className="buttons-box">
                {/* <button onClick={()=>{addToCart(product.id)}}>Add to Cart</button> */}
                <AddToCartButton
                        variant="contained"
                        disabled={isLoading}
                        onClick={() => {addToCart(product.id)}}
                        className="addproduct-btn"
                    >
                        {isLoading ? <CircularProgress size={24} /> : 'Add to Cart'}
                    </AddToCartButton>

                {/* <Link to="/shop"><button>Back to Shop</button></Link> */}
                <Link to="/shop">
                <BackToShopButton variant="contained" disabled={isLoading} className="backtoshop-btn">
                {isLoading ? <CircularProgress size={24} /> : 'Back to Shop'}
                </BackToShopButton>
                </Link>
                </div>
            </div>
        </div>
    )
}
export default ProductDisplay;