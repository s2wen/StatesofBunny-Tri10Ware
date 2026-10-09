using UnityEngine;

public enum States
{
    Solid,
    Liquid,
    Gas,
}

public class PlayerAttributes : MonoBehaviour
{
    private int _health;

    private States _state = States.Solid;

    // attributes related to state
    private int _strength = 1;
    private float _speed = 1;
    private float _size = 1;
    private bool _flight;

    //temp variable to see state change
    private Color _targetColor = Color.red;

    /*
        should call this in player control where upon pressing a button you
        pass it a new States value. could just cycle through them (i.e. solid ->
        liquid -> gas -> solid) or have different buttons for different states
    */
    public void setState(States state)
    {
        _state = state;

        if (_state == States.Solid)
        {
            // update attributes, currently arbitrary values
            _strength = 1;
            _speed = 1;
            _size = 1;
            _flight = false;
            _targetColor = Color.red;
        }
        else if (_state == States.Liquid)
        {
            // just thinking of using size as a scalar for shrinking the sprite
            // and speed as a scalar for the unit vector

            // update attributes, currently arbitrary values
            _strength = 0;
            _speed = 1.5f;
            _size = 0.5f;
            _flight = false;
            _targetColor = Color.blue;
        }
        else if (_state == States.Gas)
        {
            // update attributes, currently arbitrary values
            _strength = 0;
            _speed = 1;
            _size = 1;
            _flight = true;
            _targetColor = Color.yellow;
        }
    }

    public States getState(){
        return _state;
    }

    public Color getColor(){
        return _targetColor;
    }

    public int getStrength(){
        return _strength;
    }

    public bool getFlight(){
        return _flight;
    }

    public float getSize(){
        return _size;
    }
}
