import 'bootstrap/dist/css/bootstrap.min.css'
function Alert(props){
    return (
        <div className={"alert alert-dismissible alert-pos "+(props.response/200===1?"alert-success ":"alert-danger ")+
            (props.response==-1 ?"non-displayed ":"")} role="alert">
            {props.message}
            {//<button className="btn-close" aria-label="close" data-bs-dismiss="alert"></button>
            }
        </div>
    );
}

export default Alert