const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Timeslots', {
    Timeslot_Id: {
      autoIncrement: true,
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true
    },
    Date: {
      type: DataTypes.DATEONLY,
      allowNull: false
    },
    Time: {
      type: DataTypes.TIME,
      allowNull: false,
      get() {
        const raw = this.getDataValue('Time');
        if (!raw) return null;

        // If Sequelize gives Date → format it in UTC so no timezone shift
        if (raw instanceof Date) {
          return raw.toISOString().substring(11, 19); // "HH:MM:SS"
        }

        return raw; // string (some versions of tedious already return as string)
     }

    }
  }, {
    sequelize,
    tableName: 'Timeslots',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Timeslots_Date_Time",
        unique: true,
        fields: [
          { name: "Date" },
          { name: "Time" },
        ]
      },
      {
        name: "PK_Timeslots",
        unique: true,
        fields: [
          { name: "Timeslot_Id" },
        ]
      },
    ]
  });
};
