import React from 'react'
import './DescriptionBox.css'

const DescriptionBox = () => {
    return (
        <div className="descriptionbox">
            <div className="descriptionbox-navigator">
                <div className="descriptionbox-nav-box">Description</div>
                <div className="descriptionbox-nav-box fade">Review (111)</div>
            </div>
            <div className="descriptionbox-description">
                <p>
                    This is a description box.
                </p>
                <p>
                    This is a review box.
                </p>
            </div>
        </div>
    )
}

export default DescriptionBox