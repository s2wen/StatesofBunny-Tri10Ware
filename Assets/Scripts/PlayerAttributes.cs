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
    private int _strength;
    private float _speed = 1;
    private float _size = 1;
    private bool _flight;

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
        }
        else if (_state == States.Gas)
        {
            // update attributes, currently arbitrary values
            _strength = 0;
            _speed = 1;
            _size = 1;
            _flight = true;
        }
    }
}
