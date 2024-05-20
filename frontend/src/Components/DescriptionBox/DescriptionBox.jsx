import React from 'react'
import './DescriptionBox.css'

const DescriptionBox = (productInput) => {
    const {product} = productInput; 
    return (
        <div className="descriptionbox">
            <div className="descriptionbox-navigator">
                <div className="descriptionbox-nav-box">Description</div>
                <div className="descriptionbox-nav-box fade">Review (111)</div>
            </div>
            <div className="descriptionbox-description">
                <p>
                    {product.description}
                </p>
            </div>
            <div className="descriptionbox-review" hidden="true">
                <p>Placeholder</p>
            </div>
        </div>
    )
}

export default DescriptionBox